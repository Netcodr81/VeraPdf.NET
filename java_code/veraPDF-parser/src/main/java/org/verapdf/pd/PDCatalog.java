/*
 * This file is part of veraPDF Parser, a module of the veraPDF project.
 * Copyright (c) 2015-2026, veraPDF Consortium <info@verapdf.org>
 * All rights reserved.
 *
 * veraPDF Parser is free software: you can redistribute it and/or modify
 * it under the terms of either:
 *
 * The GNU General public license GPLv3+.
 * You should have received a copy of the GNU General Public License
 * along with veraPDF Parser as the LICENSE.GPL file in the root of the source
 * tree.  If not, see http://www.gnu.org/licenses/ or
 * https://www.gnu.org/licenses/gpl-3.0.en.html.
 *
 * The Mozilla Public License MPLv2+.
 * You should have received a copy of the Mozilla Public License along with
 * veraPDF Parser as the LICENSE.MPL file in the root of the source tree.
 * If a copy of the MPL was not distributed with this file, you can obtain one at
 * http://mozilla.org/MPL/2.0/.
 */
package org.verapdf.pd;

import org.verapdf.as.ASAtom;
import org.verapdf.cos.COSArray;
import org.verapdf.cos.COSObjType;
import org.verapdf.cos.COSObject;
import org.verapdf.pd.actions.PDAction;
import org.verapdf.pd.actions.PDCatalogAdditionalActions;
import org.verapdf.pd.form.PDAcroForm;
import org.verapdf.pd.optionalcontent.PDOptionalContentProperties;
import org.verapdf.pd.structure.PDNumberTreeNode;
import org.verapdf.pd.structure.PDStructTreeRoot;
import org.verapdf.tools.PageLabels;

import java.util.ArrayList;
import java.util.Collections;
import java.util.List;
import java.util.logging.Level;
import java.util.logging.Logger;

/**
 * High-level view of the PDF catalog dictionary, the root of the document's
 * page tree and other document-wide structures.
 *
 * <p>The catalog provides access to optional features such as metadata,
 * outlines, forms, output intents, name trees, and tagged-PDF structure.</p>
 *
 *
 * <h2>.NET porting notes</h2>
 * In C#, {@code PDCatalog} is an orchestration/facade boundary over COS storage. Inject parsing,
 * resource-resolution, and document-services dependencies; return nullable domain wrappers for
 * optional PDF keys and implement {@code IDisposable} where the facade owns streams, readers, or
 * caches.
 *
 * @author Timur Kamalov
 */
public class PDCatalog extends PDObject {

	private static final Logger LOGGER = Logger.getLogger(PDCatalog.class.getCanonicalName());

	private final PDPageTree pages;
	private PDNamesDictionary namesDictionary;
	private PDNumberTreeNode pageLabelsTree;
	private PageLabels pageLabels;

	public PDCatalog() {
		super();
		this.pages = new PDPageTree();
	}

	public PDCatalog(final COSObject object) {
		super(object);
		this.pages = new PDPageTree();
	}

	/**
	 * Returns the root of the document's page tree, resolving it from the
	 * catalog the first time it is requested.
	 *
	 * @return the catalog page tree
	 */
	public PDPageTree getPageTree() {
		if (pages.empty()) {
			final COSObject pages = super.getObject().getKey(ASAtom.PAGES);
			if (pages != null) {
				this.pages.setObject(pages);
			}
		}
		return pages;
	}

	/**
	 * Returns the catalog metadata stream when present and correctly typed.
	 *
	 * @return metadata wrapper, or {@code null} if there is no metadata stream
	 */
	public PDMetadata getMetadata() {
		COSObject metadata = getKey(ASAtom.METADATA);
		if (metadata != null && metadata.getType() == COSObjType.COS_STREAM) {
			return new PDMetadata(metadata);
		}
		return null;
	}

	public PDStructTreeRoot getStructTreeRoot() {
		COSObject base = getKey(ASAtom.STRUCT_TREE_ROOT);
		if (base != null && base.getType() == COSObjType.COS_DICT) {
			return new PDStructTreeRoot(base);
		}
		return null;
	}

	public List<PDOutputIntent> getOutputIntents() {
		COSObject base = getKey(ASAtom.OUTPUT_INTENTS);
		if (base != null && base.getType() == COSObjType.COS_ARRAY) {
			if (base.isIndirect()) {
				base = base.getDirect();
			}
			List<PDOutputIntent> result = new ArrayList<>(base.size());
			for (COSObject obj : (COSArray) base.getDirectBase()) {
				if (obj != null && obj.getType().isDictionaryBased()) {
					result.add(new PDOutputIntent(obj));
				}
			}
			return Collections.unmodifiableList(result);
		}
		return Collections.emptyList();
	}

	public PDOptionalContentProperties getOCProperties() {
		COSObject object = getKey(ASAtom.OCPROPERTIES);
		if (object != null && !object.empty() && object.getType() == COSObjType.COS_DICT) {
			return new PDOptionalContentProperties(object);
		}
		return null;
	}

	public PDOutlineDictionary getOutlines() {
		COSObject outlines = getKey(ASAtom.OUTLINES);
		if (outlines != null && outlines.getType().isDictionaryBased()) {
			return new PDOutlineDictionary(outlines);
		}
		return null;
	}

	public PDAction getOpenAction() {
		COSObject openAction = getKey(ASAtom.OPEN_ACTION);
		if (openAction != null && openAction.getType() == COSObjType.COS_DICT) {
			return new PDAction(openAction);
		}
		return null;
	}

	public COSObject getDests() {
		return getKey(ASAtom.DESTS);
	}

	public PDCatalogAdditionalActions getAdditionalActions() {
		COSObject aaDict = getKey(ASAtom.AA);
		if (aaDict != null && aaDict.getType() == COSObjType.COS_DICT) {
			return new PDCatalogAdditionalActions(aaDict);
		}
		return null;
	}

	public PDAcroForm getAcroForm() {
		COSObject acroForm = getKey(ASAtom.ACRO_FORM);
		if (acroForm != null && acroForm.getType().isDictionaryBased()) {
			return new PDAcroForm(acroForm);
		}
		return null;
	}

	public PDNamesDictionary getNamesDictionary() {
		if (namesDictionary == null) {
			COSObject buffer = getKey(ASAtom.NAMES);
			if (buffer != null && buffer.getType() == COSObjType.COS_DICT) {
				namesDictionary = new PDNamesDictionary(buffer);
			}
		}
		return namesDictionary;
	}

	public PageLabels getPageLabels() {
		if (pageLabels == null) {
			PDNumberTreeNode pageLabelsTree = getPageLabelsTree();
			if (pageLabelsTree != null) {
				pageLabels = new PageLabels(pageLabelsTree);
			}
		}
		return null;
	}
	
	public PDNumberTreeNode getPageLabelsTree() {
		if (pageLabelsTree == null) {
			COSObject labelsTree = getKey(ASAtom.PAGE_LABELS);
			if (labelsTree != null && !labelsTree.empty() && labelsTree.getType() == COSObjType.COS_DICT) {
				pageLabelsTree = new PDNumberTreeNode(labelsTree);
			}
		}
		return pageLabelsTree;
	}

	public String getVersion() {
		COSObject version = getKey(ASAtom.VERSION);
		if (version == null || version.empty()) {
			return null;
		}
		if (version.getType() != COSObjType.COS_NAME) {
			LOGGER.log(Level.WARNING, "Entry Version in the catalog does not have type name");
			return null;
		}
		return version.getString();
	}

	public Float getFloatVersion() {
		try {
			String version = getVersion();
			if (version != null) {
				return Float.parseFloat(version);
			} else {
				return null;
			}
		}
		catch (NumberFormatException e) {
			LOGGER.log(Level.WARNING, "Entry Version in the catalog cannot be converted to number");
			return null;
		}
	}

}
