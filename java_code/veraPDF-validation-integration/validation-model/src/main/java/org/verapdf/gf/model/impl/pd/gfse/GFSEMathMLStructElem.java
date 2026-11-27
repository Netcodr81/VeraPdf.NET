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
package org.verapdf.gf.model.impl.pd.gfse;

import org.verapdf.cos.COSKey;
import org.verapdf.gf.model.impl.pd.GFPDStructElem;
import org.verapdf.pd.structure.PDStructElem;
import org.verapdf.model.selayer.SEMathMLStructElem;
import org.verapdf.pd.structure.StructureType;
import org.verapdf.tools.StaticResources;
import org.verapdf.tools.TaggedPDFConstants;
import org.verapdf.tools.TaggedPDFRoleMapHelper;

import java.util.HashSet;
import java.util.Set;
import java.util.logging.Level;
import java.util.logging.Logger;

/**
 * Adapts the MathML structure role and checks whether a formula-like ancestor contains it.
 * <p>{@link #gethasParentFormulaOrMathML()} walks parent structure elements, accepting an ancestor whose mapped
 * standard role is {@code Formula} or a MathML role. It tracks COS object keys to stop malformed parent cycles
 * and logs a warning rather than looping forever.</p>
 * <p><b>.NET porting notes:</b> implement {@code VeraPdf.Model.SELayer.ISEMathMLStructElem} on
 * {@code VeraPdf.Validation.Gf.Model.Impl.Pd.Gfse.GFSEMathMLStructElem}. Use a {@code HashSet<COSKey>} equivalent
 * for cycle detection and {@code ILogger<T>} instead of {@code java.util.logging}.</p>
 */
public class GFSEMathMLStructElem extends GFPDStructElem implements SEMathMLStructElem {

    private static final Logger LOGGER = Logger.getLogger(GFSEMathMLStructElem.class.getCanonicalName());

    public static final String MATH_ML_STRUCTURE_ELEMENT_TYPE = "SEMathMLStructElem";

    public GFSEMathMLStructElem(PDStructElem structElemDictionary) {
        super(structElemDictionary, TaggedPDFConstants.MATH_ML, MATH_ML_STRUCTURE_ELEMENT_TYPE);
    }

    @Override
    public Boolean gethasParentFormulaOrMathML() {
        TaggedPDFRoleMapHelper taggedPDFRoleMapHelper = StaticResources.getRoleMapHelper();
        org.verapdf.pd.structure.PDStructElem parent = ((org.verapdf.pd.structure.PDStructElem) simplePDObject).getParent();
        if (parent != null && taggedPDFRoleMapHelper != null) {
            Set<COSKey> keys = new HashSet<>();
            while (parent != null) {
                StructureType standardStructureType = PDStructElem.getStructureElementStandardStructureType(parent);
                if ((standardStructureType != null && TaggedPDFConstants.FORMULA.equals(standardStructureType.getType().getValue())) || PDStructElem.isMathStandardType(standardStructureType)) {
                    return true;
                }
                COSKey key = parent.getObject().getObjectKey();
                if (keys.contains(key)) {
                    LOGGER.log(Level.WARNING, "Struct tree loop found");
                    break;
                }
                if (key != null) {
                    keys.add(key);
                }
                parent = parent.getParent();
            }
        }
        return false;
    }
}
