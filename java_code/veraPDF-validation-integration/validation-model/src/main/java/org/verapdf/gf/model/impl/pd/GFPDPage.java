/*
 * This file is part of veraPDF Validation, a module of the veraPDF project.
 * Copyright (c) 2015-2026, veraPDF Consortium <info@verapdf.org>
 * All rights reserved.
 *
 * veraPDF Validation is free software: you can redistribute it and/or modify
 * it under the terms of either:
 *
 * The GNU General public license GPLv3+.
 * You should have received a copy of the GNU General Public License
 * along with veraPDF Validation as the LICENSE.GPL file in the root of the source
 * tree.  If not, see http://www.gnu.org/licenses/ or
 * https://www.gnu.org/licenses/gpl-3.0.en.html.
 *
 * The Mozilla Public License MPLv2+.
 * You should have received a copy of the Mozilla Public License along with
 * veraPDF Validation as the LICENSE.MPL file in the root of the source tree.
 * If a copy of the MPL was not distributed with this file, you can obtain one at
 * http://mozilla.org/MPL/2.0/.
 */
package org.verapdf.gf.model.impl.pd;

import org.verapdf.as.ASAtom;
import org.verapdf.cos.COSArray;
import org.verapdf.gf.model.impl.containers.StaticContainers;
import org.verapdf.gf.model.impl.cos.GFCosBBox;
import org.verapdf.gf.model.impl.pd.actions.GFPDAdditionalActions;
import org.verapdf.pd.PDResourcesHandler;
import org.verapdf.model.baselayer.Object;
import org.verapdf.model.coslayer.CosBBox;
import org.verapdf.model.pdlayer.*;
import org.verapdf.pd.PDAnnotation;
import org.verapdf.pd.actions.PDPageAdditionalActions;
import org.verapdf.pd.colors.PDColorSpace;
import org.verapdf.pd.structure.StructureElementAccessObject;
import org.verapdf.pdfa.flavours.PDFAFlavour;
import org.verapdf.pdfa.flavours.PDFFlavours;

import java.util.ArrayList;
import java.util.Collections;
import java.util.List;

/**
 * Document-page validation-model adapter representing one PDF page dictionary and inherited resources.
 *
 * <p>A PDF page combines a media/crop/bleed/trim/art box, inherited resources, content stream,
 * annotations, transparency group, additional actions, and output intents.  The class lazily parses
 * content and annotations, detects transparency, derives orientation, and links page boxes and resources.
 *  Page semantics and inherited attributes follow ISO 32000-1/2.</p>
 *
 * <p><b>.NET porting notes:</b></p>
 * <ul>
 *   <li>Suggested C# type: {@code VeraPdf.Validation.Gf.Model.Impl.Pd.GFPDPage}, implementing
 *       {@code VeraPdf.Model.PDLayer.IPDPage}.</li>
 *   <li>Map {@code PDPage}, {@code PDResourcesHandler}, and COS boxes to
 *       {@code VeraPdf.Parser.Pd.PdfPage/PdfResources} and {@code CosArray}; return IReadOnlyList links.</li>
 *   <li>Keep lazy caches explicit (content streams, annotations, output intents) and thread/scoped in C#.
 *       Static transparency containers should become scoped validation context state.</li>
 *   <li>Orientation logic compares page-box dimensions and rotation; rotation is a multiple of 90 degrees
 *       in PDF, so avoid floating-point rounding changes and retain the odd Java modulo behavior exactly.</li>
 * </ul>
 *
 * @author Sergey Shemyakov
 */
public class GFPDPage extends GFPDObject implements PDPage {

	/**
	 * Type name for GFPDPage
	 */
	public static final String PD_PAGE_TYPE = "PDPage";

	/**
	 * Link name for page annotations
	 */
	private static final String ANNOTS = "annots";
	/**
	 * Link name for page additional actions
	 */
	private static final String ACTION = "AA";
	/**
	 * Link name for page content stream
	 */
	private static final String CONTENT_STREAM = "contentStream";
	/**
	 * Link name for page transparency group
	 */
	private static final String GROUP = "Group";
	/**
	 * Link name for page media box
	 */
	private static final String MEDIA_BOX = "MediaBox";
	/**
	 * Link name for page crop box
	 */
	private static final String CROP_BOX = "CropBox";
	/**
	 * Link name for page bleed box
	 */
	private static final String BLEED_BOX = "BleedBox";
	/**
	 * Link name for trim media box
	 */
	private static final String TRIM_BOX = "TrimBox";
	/**
	 * Link name for page art box
	 */
	private static final String ART_BOX = "ArtBox";
	/**
	 * Link name for all output intents
	 */
	public static final String OUTPUT_INTENTS = "outputIntents";
	/**
	 * Link name for font names, names of colourants in Separation and DeviceN colour spaces
	 */
	private static final String RESOURCES = "resources";

	public static final String PORTRAIT_ORIENTATION = "Portrait";
	public static final String LANDSCAPE_ORIENTATION = "Landscape";
	public static final String SQUARE_ORIENTATION = "Square";

	public static final String TRANSPARENCY_COLOR_SPACE = "transparencyColorSpace";
	public static final String PARENT_TRANSPARENCY_COLOR_SPACE = "parentTransparencyColorSpace";

	private boolean containsTransparency = false;
	private List<PDContentStream> contentStreams = null;
	private List<PDAnnot> annotations = null;
	private OutputIntents outputIntents = null;
	private final PDColorSpace blendingColorSpace;

	/**
	 * Default constructor
	 *
	 * @param pdPage is greenfield parser PDPage.
	 */
	public GFPDPage(org.verapdf.pd.PDPage pdPage) {
		super(pdPage, PD_PAGE_TYPE);
		this.blendingColorSpace = getBlendingColorSpace();
	}

	@Override
	/**
	 * Dispatches page links for group, annotations, actions, content, page boxes, output intents,
	 * transparency color spaces, and resources.  Linked objects are created lazily where parsing
	 * content or annotations is expensive.
	 *
	 * @param link validation-profile link name
	 * @return one or more matching model objects, or the superclass result for unknown names
	 */
	public List<? extends Object> getLinkedObjects(String link) {
		switch (link) {
			case GROUP:
				return this.getGroup();
			case ANNOTS:
				return this.getAnnotations();
			case ACTION:
				return this.getActions();
			case CONTENT_STREAM:
				return this.getContentStream();
			case MEDIA_BOX:
				return this.getMediaBox();
			case CROP_BOX:
				return this.getCropBox();
			case BLEED_BOX:
				return this.getBleedBox();
			case TRIM_BOX:
				return this.getTrimBox();
			case ART_BOX:
				return this.getArtBox();
			case OUTPUT_INTENTS:
				return this.getOutputIntents();
			case TRANSPARENCY_COLOR_SPACE:
				return this.getTransparencyColorSpace();
			case PARENT_TRANSPARENCY_COLOR_SPACE:
				return this.getParentTransparencyColorSpace();
			case RESOURCES:
				return this.getResources();
			default:
				return super.getLinkedObjects(link);
		}
	}

	private List<OutputIntents> getOutputIntents() {
		if (this.outputIntents == null) {
			this.outputIntents = parseOutputIntents();
		}
		if (this.outputIntents != null) {
			List<OutputIntents> outIntents = new ArrayList<>(MAX_NUMBER_OF_ELEMENTS);
			outIntents.add(this.outputIntents);
			return outIntents;
		}
		return Collections.emptyList();
	}

	private OutputIntents parseOutputIntents() {
		if (!PDFFlavours.isFlavourPart(StaticContainers.getFlavour(), PDFAFlavour.Specification.ISO_19005_4)) {
			return null;
		}
		List<org.verapdf.pd.PDOutputIntent> outInts = ((org.verapdf.pd.PDPage) this.simplePDObject).getOutputIntents();
		if (!outInts.isEmpty()) {
			return new GFOutputIntents(outInts);
		}
		return null;
	}

	private List<PDGroup> getGroup() {
		org.verapdf.pd.PDPage page = (org.verapdf.pd.PDPage) this.simplePDObject;
		org.verapdf.pd.PDGroup group = page.getGroup();
		if (group != null) {
			List<PDGroup> res = new ArrayList<>(MAX_NUMBER_OF_ELEMENTS);
			res.add(new GFPDGroup(group, page.getResources()));
			return Collections.unmodifiableList(res);
		}
		return Collections.emptyList();
	}

	private List<PDAnnot> getAnnotations() {
		if (this.annotations == null) {
			this.annotations = parseAnnotations();
		}

		return this.annotations;
	}

	@Override
	public Boolean getcontainsAnnotations() {
		return !((org.verapdf.pd.PDPage) simplePDObject).getAnnotations().isEmpty();
	}

	private List<PDAnnot> parseAnnotations() {
		StaticContainers.getTransparencyVisitedContentStreams().clear();
		List<PDAnnotation> annots = ((org.verapdf.pd.PDPage) simplePDObject).getAnnotations();
		if (!annots.isEmpty()) {
			List<PDAnnot> res = new ArrayList<>(annots.size());
			org.verapdf.pd.PDPage page = (org.verapdf.pd.PDPage) this.simplePDObject;
			PDResourcesHandler resourcesHandler = PDResourcesHandler.getInstance(page.getResources(), page.isInheritedResources());
			for (PDAnnotation annot : annots) {
				GFPDAnnot annotation = GFPDAnnot.createAnnot(annot, resourcesHandler, page);
				this.containsTransparency |= annotation.isContainsTransparency();
				res.add(annotation);
			}
			return Collections.unmodifiableList(res);
		}
		return Collections.emptyList();
	}

	private List<PDAdditionalActions> getActions() {
		PDPageAdditionalActions additionalActions =
				((org.verapdf.pd.PDPage) this.simplePDObject).getAdditionalActions();
		if (additionalActions != null) {
			List<PDAdditionalActions> actions = new ArrayList<>(MAX_NUMBER_OF_ELEMENTS);
			actions.add(new GFPDAdditionalActions(additionalActions));
			return Collections.unmodifiableList(actions);
		}
		return Collections.emptyList();
	}

	private List<PDContentStream> getContentStream() {
		if (this.contentStreams == null) {
			this.contentStreams = parseContentStream();
		}
		return this.contentStreams;
	}

	private List<PDContentStream> parseContentStream() {
		// Clear the visited-stream guard before page parsing.  PDF/UA uses the semantic subclass,
		// while other flavours use the ordinary operator parser.
		StaticContainers.getTransparencyVisitedContentStreams().clear();
		List<PDContentStream> pdContentStreams = new ArrayList<>(MAX_NUMBER_OF_ELEMENTS);
		org.verapdf.pd.PDPage page = (org.verapdf.pd.PDPage) this.simplePDObject;
		if (page.getContent() != null) {
			PDResourcesHandler resourcesHandler = PDResourcesHandler.getInstance(page.getResources(), page.isInheritedResources());
			GFPDContentStream pdContentStream;
			if (!PDFFlavours.isPDFUARelatedFlavour(StaticContainers.getFlavour())) {
				pdContentStream = new GFPDContentStream(page.getContent(), resourcesHandler, null,
						new StructureElementAccessObject(this.simpleCOSObject), page.getObject().getObjectKey());
			} else {
				pdContentStream = new GFPDSemanticContentStream(page.getContent(), resourcesHandler, null,
						new StructureElementAccessObject(this.simpleCOSObject), page.getObject().getObjectKey());
			}
			this.containsTransparency |= pdContentStream.isContainsTransparency();
			pdContentStreams.add(pdContentStream);
		}
		return pdContentStreams;
	}

	private List<CosBBox> getMediaBox() {
		return getBBox(((org.verapdf.pd.PDPage) simplePDObject).getCOSMediaBox());
	}

	private List<CosBBox> getCropBox() {
		return getBBox(((org.verapdf.pd.PDPage) simplePDObject).getCOSCropBox());
	}

	private List<CosBBox> getBleedBox() {
		return getBBox(((org.verapdf.pd.PDPage) simplePDObject).getCOSBleedBox());
	}

	private List<CosBBox> getTrimBox() {
		return getBBox(((org.verapdf.pd.PDPage) simplePDObject).getCOSTrimBox());
	}

	private List<CosBBox> getArtBox() {
		return getBBox(((org.verapdf.pd.PDPage) simplePDObject).getCOSArtBox());
	}

	private static List<CosBBox> getBBox(COSArray array) {
		if (array != null) {
			List<CosBBox> res = new ArrayList<>(MAX_NUMBER_OF_ELEMENTS);
			res.add(new GFCosBBox(array));
			return Collections.unmodifiableList(res);
		}
		return Collections.emptyList();
	}

	/**
	 * @return true if the page contains presentation steps
	 * (/PresSteps in the page dictionary).
	 */
	@Override
	public Boolean getcontainsPresSteps() {
		return ((org.verapdf.pd.PDPage) simplePDObject).getCOSPresSteps() != null;
	}

	/**
	 * @return true if the page contains transparency.
	 */
	@Override
	public Boolean getcontainsTransparency() {
		StaticContainers.setCurrentTransparencyColorSpace(blendingColorSpace);
		if (this.contentStreams == null) {
			this.contentStreams = parseContentStream();
		}
		if (this.annotations == null) {
			this.annotations = parseAnnotations();
		}
		return this.containsTransparency;
	}

	/**
	 * @return true if the page contains group which contains color space
	 */
	@Override
	public Boolean getcontainsGroupCS() {
		org.verapdf.pd.PDGroup group = ((org.verapdf.pd.PDPage) this.simplePDObject).getGroup();
		return group != null && group.getColorSpace() != null;
	}

	@Override
	public Boolean getcontainsAA() {
		return this.simplePDObject == null ? Boolean.FALSE :
				this.simplePDObject.knownKey(ASAtom.AA);
	}

	@Override
	public String getTabs() {
		return ((org.verapdf.pd.PDPage)this.simplePDObject).getTabs();
	}

	@Override
	public String getorientation() {
		// MediaBox dimensions and page rotation determine the document-facing orientation.  A C# port
		// should keep the modulo/height-width comparisons and avoid using 90-degree floating rounding.
		CosBBox mediaBox = new GFCosBBox(((org.verapdf.pd.PDPage) simplePDObject).getCOSMediaBox());
		double height = mediaBox.gettop() - mediaBox.getbottom();
		double width = mediaBox.getright() - mediaBox.getleft();
		long rotation = ((org.verapdf.pd.PDPage) simplePDObject).getRotation();
		if ((height > width && rotation % 180 == 0) || (height < width && rotation % 180 == 90)) {
			return PORTRAIT_ORIENTATION;
		}
		if ((height < width && rotation % 180 == 0) || (height > width && rotation % 180 == 90)) {
			return LANDSCAPE_ORIENTATION;
		}
		return SQUARE_ORIENTATION;
	}

	@Override
	public String getoutputColorSpace() {
		if (this.outputIntents == null) {
			this.outputIntents = parseOutputIntents();
		}
		if (this.outputIntents != null) {
			return ((GFOutputIntents)outputIntents).getColorSpace();
		}
		return null;
	}

	@Override
	public Long getpageNumber() {
		return (long) ((org.verapdf.pd.PDPage) this.simplePDObject).getPageNumber();
	}

	public PDColorSpace getBlendingColorSpace() {
		org.verapdf.pd.PDGroup group = ((org.verapdf.pd.PDPage) this.simplePDObject).getGroup();
		if (group == null || !ASAtom.TRANSPARENCY.equals(group.getSubtype())) {
			return null;
		}
		return group.getColorSpace();
	}

	private List<TransparencyColorSpace> getTransparencyColorSpace() {
		if (blendingColorSpace != null) {
			List<TransparencyColorSpace> xFormTransparencyGroup = new ArrayList<>(MAX_NUMBER_OF_ELEMENTS);
			xFormTransparencyGroup.add(new GFTransparencyColorSpace(blendingColorSpace));
			return Collections.unmodifiableList(xFormTransparencyGroup);
		}
		return Collections.emptyList();
	}

	private List<TransparencyColorSpace> getParentTransparencyColorSpace() {
		if (blendingColorSpace != null) {
			List<TransparencyColorSpace> parentXFormTransparencyGroup = new ArrayList<>(MAX_NUMBER_OF_ELEMENTS);
			parentXFormTransparencyGroup.add(new GFTransparencyColorSpace(null));
			StaticContainers.setCurrentTransparencyColorSpace(blendingColorSpace);
			return Collections.unmodifiableList(parentXFormTransparencyGroup);
		}
		return Collections.emptyList();
	}

	private List<PDResources> getResources() {
		List<PDResources> result = new ArrayList<>(MAX_NUMBER_OF_ELEMENTS);
		org.verapdf.pd.PDResources resources = ((org.verapdf.pd.PDPage) this.simplePDObject).getResources();
		if (resources != null) {
			result.add(new GFPDResources(resources));
		}
		return result;
	}
}
